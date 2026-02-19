import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router, NavigationEnd } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-dashboardlayout',
  standalone: false,
  templateUrl: './dashboardlayout.component.html',
  styleUrls: ['./dashboardlayout.component.css']
})
export class DashboardlayoutComponent implements OnInit {
  userRole: string | null = null;
  userName: string | null = null;
  userProfileImage: string | null = null;
  isSidebarOpen = true; // Default to open for desktop
  isMobile = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private breakpointObserver: BreakpointObserver
  ) { }

  ngOnInit(): void {
    this.userRole = this.authService.getRole();
    this.userName = this.authService.getFullName();
    const user = this.authService.getCurrentUser();
    this.userProfileImage = user?.profilePictureUrl || null;

    // Responsive Breakpoints
    this.breakpointObserver.observe([
      '(max-width: 768px)'
    ]).subscribe(result => {
      this.isMobile = result.matches;
      if (this.isMobile) {
        this.isSidebarOpen = false; // Close on mobile by default
      } else {
        this.isSidebarOpen = true; // Open on desktop
      }
    });

    // Close sidebar on route change if mobile
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.isSidebarOpen = false;
      document.body.classList.remove('no-scroll');
    });
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  closeSidebar(): void {
    this.isSidebarOpen = false;
    document.body.classList.remove('no-scroll');
  }

  logout(): void {
    this.authService.logout();
  }
}
