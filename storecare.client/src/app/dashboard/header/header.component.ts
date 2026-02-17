import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-header',
  standalone: false,
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent {
  @Input() userName: string | null = null;
  @Input() userRole: string | null = null;
  @Input() userProfileImage: string | null = null;
  @Output() logout = new EventEmitter<void>();
  @Output() sidebarToggle = new EventEmitter<void>(); // Add this

  showUserMenu = false;

  constructor(
    private router: Router,
    private authService: AuthService
  ) { }

  toggleUserMenu(): void {
    this.showUserMenu = !this.showUserMenu;
  }

  toggleSidebar(): void {
    this.sidebarToggle.emit(); // Emit event instead of directly manipulating DOM
  }

  onLogout(): void {
    this.logout.emit();
  }

  goToProfile(): void {
    this.router.navigate(['/dashboard/profile']);
    this.showUserMenu = false;
  }

  goToSettings(): void {
    this.router.navigate(['/dashboard/settings']);
    this.showUserMenu = false;
  }
}
