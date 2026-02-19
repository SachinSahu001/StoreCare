import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { GlobalErrorHandler } from './shared/handlers/global-error-handler';
import { ErrorHandler } from '@angular/core';

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
import { SharedModule } from './shared/shared.module';

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
    BrowserAnimationsModule,
    HttpClientModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,

    CommonModule,
    SharedModule
  ],
  providers: [
    AuthGuard,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    { provide: ErrorHandler, useClass: GlobalErrorHandler }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
