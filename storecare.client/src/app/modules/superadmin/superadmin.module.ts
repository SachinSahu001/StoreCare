import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SuperadminRoutingModule } from './superadmin-routing.module';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ManageCategoriesComponent } from './manage-categories/manage-categories.component';
import { ManageProductsComponent } from './manage-products/manage-products.component';
import { ManageStoresComponent } from './manage-stores/manage-stores.component';
import { ManageAssignmentsComponent } from './manage-assignments/manage-assignments.component';
import { ManageUsersComponent } from './manage-users/manage-users.component';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

// Angular Material Imports
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { CategoryDialogComponent } from './manage-categories/category-dialog/category-dialog.component';
import { ProductDialogComponent } from './manage-products/product-dialog/product-dialog.component';
import { StoreDialogComponent } from './manage-stores/store-dialog/store-dialog.component';
import { StoreAdminDialogComponent } from './manage-stores/store-admin-dialog/store-admin-dialog.component';


@NgModule({
  declarations: [
    DashboardComponent,
    ManageCategoriesComponent,
    ManageProductsComponent,
    ManageStoresComponent,
    ManageAssignmentsComponent,
    ManageUsersComponent,
    CategoryDialogComponent,
    ProductDialogComponent,
    StoreDialogComponent,
    StoreAdminDialogComponent
  ],
  imports: [
    CommonModule,
    SuperadminRoutingModule,
    ReactiveFormsModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatButtonModule,
    MatInputModule,
    MatIconModule,
    MatSelectModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatCardModule
  ]
})
export class SuperadminModule { }
