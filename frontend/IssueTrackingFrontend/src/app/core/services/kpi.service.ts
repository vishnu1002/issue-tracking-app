import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DashboardStats, RepresentativePerformance, KPIFilters } from '../../models/kpi.model';
import { env } from '../../../env/env';

@Injectable({
  providedIn: 'root',
})
export class KPIService {
  private readonly baseUrl = env.apiUrl + '/kpi';

  constructor(private http: HttpClient) {}

  // Get dashboard statistics (Admin only)
  getDashboardStats(filters?: KPIFilters): Observable<DashboardStats> {
    let params = new HttpParams();
    if (filters?.fromDate) {
      params = params.set('fromDate', filters.fromDate);
    }
    if (filters?.toDate) {
      params = params.set('toDate', filters.toDate);
    }
    return this.http.get<DashboardStats>(`${env.apiUrl}/dashboard/stats`, { params });
  }

  // Get ticket trends (Admin only)
  getTicketTrends(filters?: KPIFilters): Observable<any> {
    let params = new HttpParams();
    if (filters?.fromDate) {
      params = params.set('fromDate', filters.fromDate);
    }
    if (filters?.toDate) {
      params = params.set('toDate', filters.toDate);
    }
    return this.http.get(`${env.apiUrl}/dashboard/trends`, { params });
  }

  // Get representative performance (Admin only)
  getRepresentativePerformance(filters?: KPIFilters): Observable<any> {
    let params = new HttpParams();
    if (filters?.fromDate) {
      params = params.set('fromDate', filters.fromDate);
    }
    if (filters?.toDate) {
      params = params.set('toDate', filters.toDate);
    }
    return this.http.get(`${env.apiUrl}/dashboard/performance`, { params });
  }

  // Get all representatives KPI (Admin only) - no date filtering
  getAllRepresentativesKPI(): Observable<RepresentativePerformance[]> {
    return this.http.get<RepresentativePerformance[]>(`${this.baseUrl}/representatives`);
  }

  // Get specific representative KPI
  getRepresentativeKPI(
    representativeId: number,
    filters?: KPIFilters
  ): Observable<RepresentativePerformance> {
    let params = new HttpParams();
    if (filters?.fromDate) {
      params = params.set('fromDate', filters.fromDate);
    }
    if (filters?.toDate) {
      params = params.set('toDate', filters.toDate);
    }
    return this.http.get<RepresentativePerformance>(
      `${this.baseUrl}/representative/${representativeId}`,
      { params }
    );
  }

  // Get average resolution time (Admin only)
  getAverageResolutionTime(
    filters?: KPIFilters
  ): Observable<{ averageResolutionTimeHours: number }> {
    let params = new HttpParams();
    if (filters?.fromDate) {
      params = params.set('fromDate', filters.fromDate);
    }
    if (filters?.toDate) {
      params = params.set('toDate', filters.toDate);
    }
    return this.http.get<{ averageResolutionTimeHours: number }>(
      `${this.baseUrl}/average-resolution-time`,
      { params }
    );
  }

  // Get total tickets resolved (Admin only)
  getTotalTicketsResolved(filters?: KPIFilters): Observable<{ totalTicketsResolved: number }> {
    let params = new HttpParams();
    if (filters?.fromDate) {
      params = params.set('fromDate', filters.fromDate);
    }
    if (filters?.toDate) {
      params = params.set('toDate', filters.toDate);
    }
    return this.http.get<{ totalTicketsResolved: number }>(`${this.baseUrl}/total-resolved`, {
      params,
    });
  }
}
