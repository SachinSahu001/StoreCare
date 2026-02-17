import { Component, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  showLayout = true;

  // List of route prefixes where layout (navbar + footer) should be hidden
  // Sirf un routes ko add karo jahan navbar/footer nahi dikhana
  hideOnRoutes = [
    '/dashboard',     // Dashboard routes ke liye - yahan sidebar dikhega
    '/admin',         // Agar koi direct admin route ho toh
    '/store',         // Agar koi direct store route ho toh
    '/account'        // Agar koi direct account route ho toh
  ];

  constructor(private router: Router) {
    // Subscribe to router events
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        // Check if current route starts with any of the paths in hideOnRoutes
        this.showLayout = !this.hideOnRoutes.some(path =>
          event.urlAfterRedirects.startsWith(path)
        );

        // Debug logs - aap production mein hata sakte ho
        console.log('📍 Current URL:', event.urlAfterRedirects);
        console.log('👁️ Show Layout (Navbar+Footer):', this.showLayout);
      });
  }

  ngOnInit() {
    // Har navigation ke end par scroll to top karein
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        window.scrollTo({
          top: 0,
          behavior: 'smooth' // Smooth scrolling
        });
      }
    });
  }
}
