import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { KPIService } from '../../core/services/kpi.service';
import { DashboardStats, RepresentativePerformance, KPIFilters } from '../../models/kpi.model';

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
  averageResolutionTime: number = 0;
  totalResolved: number = 0;
  loading = false;
  error: string | null = null;

  // Date filters
  fromDate: string = '';
  toDate: string = '';

  constructor(private kpiService: KPIService) {
    // Set default date range to last 30 days
    const today = new Date();
    const thirtyDaysAgo = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000);
    this.toDate = today.toISOString().split('T')[0];
    this.fromDate = thirtyDaysAgo.toISOString().split('T')[0];
  }

  ngOnInit() {
    this.loadAnalytics();
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
    this.kpiService.getAllRepresentativesKPI(filters).subscribe({
      next: (kpis) => {
        this.representativesKPI = kpis;
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
