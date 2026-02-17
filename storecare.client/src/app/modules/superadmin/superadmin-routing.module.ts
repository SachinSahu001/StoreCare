import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { DashboardComponent } from './dashboard/dashboard.component';
import { ManageCategoriesComponent } from './manage-categories/manage-categories.component';
import { ManageProductsComponent } from './manage-products/manage-products.component';
import { ManageStoresComponent } from './manage-stores/manage-stores.component';
import { ManageAssignmentsComponent } from './manage-assignments/manage-assignments.component';
import { ManageUsersComponent } from './manage-users/manage-users.component';

const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent, title: 'SuperAdmin Dashboard' },
  { path: 'categories', component: ManageCategoriesComponent, title: 'Manage Categories' },
  { path: 'products', component: ManageProductsComponent, title: 'Manage Products' },
  { path: 'stores', component: ManageStoresComponent, title: 'Manage Stores' },
  { path: 'assignments', component: ManageAssignmentsComponent, title: 'Manage Assignments' },
  { path: 'users', component: ManageUsersComponent, title: 'Manage Users' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SuperadminRoutingModule { }
