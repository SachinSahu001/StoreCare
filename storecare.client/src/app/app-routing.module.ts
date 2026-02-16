import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NavbarComponent } from './navbar/navbar.component';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './register/register.component';
import { FooterComponent } from './footer/footer.component';

const routes: Routes = [
  { path: 'Navbar', component: NavbarComponent },
  { path: 'Home', component: HomeComponent },
  { path: 'Footer', component: FooterComponent },
  { path: 'Register', component: RegisterComponent },
  { path: 'Login', component: LoginComponent },


  { path: '', component: HomeComponent },

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
