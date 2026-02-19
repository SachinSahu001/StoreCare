import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit {
  isMenuOpen = false;
  isLoggedIn = false;
  userRole: string | null = null;
  userName: string | null = null;
  userProfileImage: string | null = null;

  constructor(public authService: AuthService, private router: Router) { }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.isLoggedIn = !!user;
      this.userRole = this.authService.getRole();
      this.userName = this.authService.getFullName();
      this.userProfileImage = user?.profilePictureUrl || null;
    });
  }

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  logout() {
    this.authService.logout();
    this.isMenuOpen = false;
  }

  getDashboardRoute(): string {
    if (this.userRole === 'SuperAdmin') return '/dashboard/superadmin';
    if (this.userRole === 'StoreAdmin') return '/dashboard/storeadmin';
    if (this.userRole === 'Customer') return '/dashboard/customer';
    return '/';
  }
}
