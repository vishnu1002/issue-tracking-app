export interface DashboardStats {
  totalTickets: number;
  openTickets: number;
  closedTickets: number;
  inProgressTickets: number;
  highPriorityTickets: number;
  totalUsers: number;
  totalRepresentatives: number;
  totalAdmins: number;
  recentTickets: number; // Last 7 days
  averageResolutionTime: number; // In hours
  ticketTrends: TicketTrend[];
  topPerformers: RepresentativePerformance[];
}

export interface TicketTrend {
  date: string;
  created: number;
  resolved: number;
}

export interface RepresentativePerformance {
  representativeId: number;
  representativeName: string;
  representativeEmail: string;
  ticketsAssigned: number;
  ticketsResolved: number;
  ticketsClosed: number;
  resolutionRate: number;
  averageResolutionTime: number;
}

export interface KPIFilters {
  fromDate?: string;
  toDate?: string;
}
