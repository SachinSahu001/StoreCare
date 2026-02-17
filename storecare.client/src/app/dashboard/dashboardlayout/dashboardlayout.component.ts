import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboardlayout',
  standalone: false,
  templateUrl: './dashboardlayout.component.html',
  styleUrls: ['./dashboardlayout.component.css']
})
export class DashboardlayoutComponent implements OnInit {
  userRole: string | null = null;
  userName: string | null = null;
  isSidebarOpen = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.userRole = this.authService.getRole();
    this.userName = this.authService.getFullName();
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  closeSidebar(): void {
    this.isSidebarOpen = false;
  }

  logout(): void {
    this.authService.logout();
  }
}
