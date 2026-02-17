import { Component, OnInit } from '@angular/core';
import { DashboardService, DashboardStats } from '../../../services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css' // Angular 17+ uses styleUrl
})
export class DashboardComponent implements OnInit {
  loading: boolean = true;
  stats: DashboardStats | null = null;

  constructor(private dashboardService: DashboardService) { }

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.loading = true;
    this.dashboardService.getSuperAdminStats().subscribe({
      next: (data: DashboardStats) => {
        this.stats = data;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading dashboard stats', error);
        this.loading = false;
      }
    });
  }
}
