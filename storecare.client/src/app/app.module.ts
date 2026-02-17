import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

// Layout Components
import { NavbarComponent } from './navbar/navbar.component';
import { FooterComponent } from './footer/footer.component';

// Auth Components
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { HomeComponent } from './home/home.component';

// Dashboard Components
import { SuperadminComponent } from './dashboard/superadmin/superadmin.component';
import { StoreadminComponent } from './dashboard/storeadmin/storeadmin.component';
import { CustomerComponent } from './dashboard/customer/customer.component';
import { DashboardlayoutComponent } from './dashboard/dashboardlayout/dashboardlayout.component';
import { SidebarComponent } from './dashboard/sidebar/sidebar.component';
import { HeaderComponent } from './dashboard/header/header.component';

// Guards and Interceptors
import { AuthGuard } from './guard/auth.guard';
import { AuthInterceptor } from './interceptors/auth.interceptor';

@NgModule({
  declarations: [
    // Main App
    AppComponent,

    // Layout Components
    NavbarComponent,
    FooterComponent,

    // Auth Components
    LoginComponent,
    RegisterComponent,
    HomeComponent,

    // Dashboard Components (all non-standalone)
    SuperadminComponent,
    StoreadminComponent,
    CustomerComponent,
    DashboardlayoutComponent,
    SidebarComponent,
    HeaderComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    CommonModule
  ],
  providers: [
    AuthGuard,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
