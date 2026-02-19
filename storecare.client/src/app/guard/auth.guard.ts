import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    if (this.authService.isLoggedIn()) {
      // Check if route has role restrictions
      const expectedRole = route.data['role'];
      const userRole = this.authService.getRole();

      if (expectedRole) {
        if (userRole === expectedRole) {
          return true;
        } else {
          // User has wrong role, redirect to appropriate dashboard
          this.redirectBasedOnRole(userRole);
          return false;
        }
      }

      // No specific role required, allow access
      return true;
    }

    // Not logged in, redirect to login with return URL
    this.router.navigate(['/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }

  private redirectBasedOnRole(role: string | null): void {
    switch (role) {
      case 'SuperAdmin':
        this.router.navigate(['/dashboard/superadmin']);
        break;
      case 'StoreAdmin':
        this.router.navigate(['/dashboard/storeadmin']);
        break;
      case 'Customer':
        this.router.navigate(['/dashboard/customer']);
        break;
      default:
        this.router.navigate(['/']);
    }
  }
}
