import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { KPIService } from '../../core/services/kpi.service';
import { DashboardStats, RepresentativePerformance, KPIFilters } from '../../models/kpi.model';
import { TicketService } from '../../core/services/ticket.service';
import { TicketModel } from '../../models/ticket.model';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './analytics.html',
  styleUrls: ['./analytics.css'],
})
export class Analytics implements OnInit {
  dashboardStats: DashboardStats | null = null;
  representativesKPI: RepresentativePerformance[] = [];
  filteredRepresentativesKPI: RepresentativePerformance[] = [];
  repSearchTerm: string = '';
  averageResolutionTime: number = 0;
  totalResolved: number = 0;
  loading = false;
  error: string | null = null;

  // Date filters
  fromDate: string = '';
  toDate: string = '';

  // Tickets grouped by representative id
  ticketsByRep: { [repId: number]: TicketModel[] } = {};
  // Accordion expansion state per representative id
  expandedRep: { [repId: number]: boolean } = {};

  constructor(
    private kpiService: KPIService,
    private title: Title,
    private ticketService: TicketService
  ) {
    // Set default date range to last 30 days
    const today = new Date();
    const thirtyDaysAgo = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000);
    this.toDate = today.toISOString().split('T')[0];
    this.fromDate = thirtyDaysAgo.toISOString().split('T')[0];
  }

  ngOnInit() {
    this.title.setTitle('Issue Tracker - Analytics');
    this.loadAnalytics();
    this.loadTickets();
  }

  loadAnalytics() {
    this.loading = true;
    this.error = null;

    const filters: KPIFilters = {
      fromDate: this.fromDate || undefined,
      toDate: this.toDate || undefined,
    };

    // Load dashboard stats
    this.kpiService.getDashboardStats(filters).subscribe({
      next: (stats) => {
        this.dashboardStats = stats;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load dashboard statistics';
        this.loading = false;
        console.error('Error loading dashboard stats:', err);
      },
    });

    // Load representatives KPI
    this.kpiService.getAllRepresentativesKPI().subscribe({
      next: (kpis) => {
        this.representativesKPI = kpis;
        this.applyRepFilter();
      },
      error: (err) => {
        console.error('Error loading representatives KPI:', err);
      },
    });

    // Load average resolution time
    this.kpiService.getAverageResolutionTime(filters).subscribe({
      next: (data) => {
        this.averageResolutionTime = data.averageResolutionTimeHours;
      },
      error: (err) => {
        console.error('Error loading average resolution time:', err);
      },
    });

    // Load total resolved
    this.kpiService.getTotalTicketsResolved(filters).subscribe({
      next: (data) => {
        this.totalResolved = data.totalTicketsResolved;
      },
      error: (err) => {
        console.error('Error loading total resolved:', err);
      },
    });
  }

  applyRepFilter() {
    const q = (this.repSearchTerm || '').trim().toLowerCase();
    if (!q) {
      this.filteredRepresentativesKPI = [...this.representativesKPI];
      return;
    }
    this.filteredRepresentativesKPI = this.representativesKPI.filter((rep) => {
      const name = (rep.representativeName || '').toLowerCase();
      const email = (rep.representativeEmail || '').toLowerCase();
      return name.includes(q) || email.includes(q);
    });
  }

  loadTickets() {
    this.ticketService.getAllTickets().subscribe({
      next: (tickets) => {
        // Group tickets by assigned representative id
        const grouped: { [repId: number]: TicketModel[] } = {};
        for (const t of tickets) {
          const repId = t.assignedToUserId ?? -1;
          if (repId === -1) continue; // skip unassigned
          if (!grouped[repId]) grouped[repId] = [];
          grouped[repId].push(t);
        }
        // Sort each rep's tickets by createdAt desc for readability
        Object.keys(grouped).forEach((key) => {
          grouped[Number(key)].sort(
            (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
          );
        });
        this.ticketsByRep = grouped;
      },
      error: (err) => {
        console.error('Error loading tickets for analytics:', err);
      },
    });
  }

  toggleRep(repId: number) {
    this.expandedRep[repId] = !this.expandedRep[repId];
  }

  // Compute time taken text for closed tickets (same logic as Tickets admin view)
  getClosedTimeTaken(ticket: TicketModel): string | null {
    const status = (ticket.status || '').toLowerCase();
    if (status !== 'closed') return null;
    const startMs = new Date(ticket.createdAt).getTime();
    const endMs = new Date(ticket.resolvedAt || ticket.updatedAt).getTime();
    if (isFinite(startMs) && isFinite(endMs) && endMs >= startMs) {
      const diffMs = endMs - startMs;
      const totalHours = diffMs / 3600000;
      if (totalHours < 1) {
        const minutes = Math.round(diffMs / 60000);
        return `Time taken: ${minutes} minutes`;
      }
      const hoursRounded = Math.round(totalHours);
      return `Time taken: ${hoursRounded} hours`;
    }
    if (ticket.resolutionTime) {
      const span = ticket.resolutionTime.trim();
      let days = 0,
        hours = 0,
        minutes = 0,
        seconds = 0;
      if (span.includes('.')) {
        const [d, rest] = span.split('.');
        days = parseInt(d, 10) || 0;
        const parts = rest.split(':');
        hours = parseInt(parts[0], 10) || 0;
        minutes = parseInt(parts[1], 10) || 0;
        seconds = parseInt(parts[2] || '0', 10) || 0;
      } else {
        const parts = span.split(':');
        hours = parseInt(parts[0], 10) || 0;
        minutes = parseInt(parts[1], 10) || 0;
        seconds = parseInt(parts[2] || '0', 10) || 0;
      }
      const totalHours = days * 24 + hours + minutes / 60 + seconds / 3600;
      if (totalHours < 1) {
        const totalMinutes = Math.round(totalHours * 60);
        return `Time taken: ${totalMinutes} minutes`;
      }
      const hoursRounded = Math.round(totalHours);
      return `Time taken: ${hoursRounded} hours`;
    }
    return null;
  }

  onDateFilterChange() {
    this.loadAnalytics();
  }

  formatHours(hours: number): string {
    if (hours < 1) {
      return `${Math.round(hours * 60)} minutes`;
    } else if (hours < 24) {
      return `${Math.round(hours * 10) / 10} hours`;
    } else {
      const days = Math.floor(hours / 24);
      const remainingHours = Math.round((hours % 24) * 10) / 10;
      return `${days} days ${remainingHours} hours`;
    }
  }

  getResolutionRateColor(rate: number): string {
    if (rate >= 80) return 'text-green-600';
    if (rate >= 60) return 'text-yellow-600';
    return 'text-red-600';
  }

  getResolutionRateBg(rate: number): string {
    if (rate >= 80) return 'bg-green-100';
    if (rate >= 60) return 'bg-yellow-100';
    return 'bg-red-100';
  }
}
