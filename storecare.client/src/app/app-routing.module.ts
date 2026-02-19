import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './guard/auth.guard';

// Public Components
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { NavbarComponent } from './navbar/navbar.component';
import { FooterComponent } from './footer/footer.component';

// Dashboard Components
import { DashboardlayoutComponent } from './dashboard/dashboardlayout/dashboardlayout.component';
import { SuperadminComponent } from './dashboard/superadmin/superadmin.component';
import { StoreadminComponent } from './dashboard/storeadmin/storeadmin.component';
import { CustomerComponent } from './dashboard/customer/customer.component';
import { ProfileComponent } from './shared/components/profile/profile.component';


const routes: Routes = [
  // Public Routes
  { path: '', component: HomeComponent, title: 'StoreCare - Home' },
  { path: 'home', redirectTo: '', pathMatch: 'full' },
  { path: 'login', component: LoginComponent, title: 'StoreCare - Sign In' },
  { path: 'Register', component: RegisterComponent, title: 'StoreCare - Create Account' },

  // These are layout components, not meant to be routes directly
  // { path: 'navbar', component: NavbarComponent }, // Remove - this is a layout component
  // { path: 'footer', component: FooterComponent }, // Remove - this is a layout component

  // Dashboard Layout with Child Routes
  {
    path: 'dashboard',
    component: DashboardlayoutComponent,
    canActivate: [AuthGuard],
    children: [
      // SuperAdmin Routes
      {
        path: 'superadmin',
        loadChildren: () => import('./modules/superadmin/superadmin.module').then(m => m.SuperadminModule),
        canActivate: [AuthGuard],
        data: { role: 'SuperAdmin', title: 'SuperAdmin Dashboard' }
      },
      // StoreAdmin Routes
      {
        path: 'storeadmin',
        component: StoreadminComponent,
        canActivate: [AuthGuard],
        data: { role: 'StoreAdmin', title: 'StoreAdmin Dashboard' }
      },
      // Customer Routes
      {
        path: 'customer',
        component: CustomerComponent,
        canActivate: [AuthGuard],
        data: { role: 'Customer', title: 'My Account' }
      },
      // Shared Profile Route
      {
        path: 'profile',
        component: ProfileComponent,
        canActivate: [AuthGuard],
        data: { title: 'My Profile' }
      },
      // Default redirect based on role will be handled by AuthGuard or component logic
      { path: '', redirectTo: 'customer', pathMatch: 'full' }
    ]
  },

  // Wildcard route - redirect to home for any unknown paths
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
