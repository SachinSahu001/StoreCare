import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { StoreService, Store } from '../../../services/store.service';
import { StoreDialogComponent } from './store-dialog/store-dialog.component';
import { StoreAdminDialogComponent } from './store-admin-dialog/store-admin-dialog.component';

@Component({
  selector: 'app-manage-stores',
  standalone: false,
  templateUrl: './manage-stores.component.html',
  styleUrl: './manage-stores.component.css'
})
export class ManageStoresComponent implements OnInit {
  displayedColumns: string[] = ['storeName', 'storeCode', 'storeEmail', 'city', 'status', 'actions'];
  dataSource!: MatTableDataSource<Store>;
  isLoading = true;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private storeService: StoreService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadStores();
  }

  loadStores(): void {
    this.isLoading = true;
    this.storeService.getStores().subscribe({
      next: (data) => {
        this.dataSource = new MatTableDataSource(data);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading stores', error);
        this.showSnackBar('Error loading stores', 'Close');
        this.isLoading = false;
      }
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  openStoreDialog(store?: Store): void {
    const dialogRef = this.dialog.open(StoreDialogComponent, {
      width: '600px',
      data: store || {}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        const formData = this.createFormData(result);
        if (store) {
          this.updateStore(store.id, formData);
        } else {
          this.createStore(formData);
        }
      }
    });
  }

  createFormData(store: any): FormData {
    const formData = new FormData();
    // Map frontend keys to Backend DTO keys (PascalCase)
    formData.append('StoreName', store.storeName);
    formData.append('Address', store.address); // Changed from storeAddress
    formData.append('ContactNumber', store.contactNumber); // Changed from storePhone
    formData.append('Email', store.email); // Changed from storeEmail

    // StatusId logic - assuming backend handles IsActive map or we send StatusId
    // If backend expects StatusId for active/inactive:
    formData.append('StatusId', store.active ? '1' : '2'); // Example: 1=Active, 2=Inactive

    if (store.storeLogo) {
      formData.append('StoreLogo', store.storeLogo);
    }
    return formData;
  }

  openAdminDialog(store: Store): void {
    this.dialog.open(StoreAdminDialogComponent, {
      width: '500px',
      data: { storeId: store.id, storeName: store.storeName }
    });
  }
  // Added property to class to fix compilation if needed, or remove 'this.dialogRef =' if not strictly needed property. 
  // Actually openAdminDialog was valid before, just fixing indentation/context.

  createStore(storeData: FormData): void {
    this.storeService.createStore(storeData).subscribe({
      next: () => {
        this.showSnackBar('Store created successfully', 'Close');
        this.loadStores();
      },
      error: (error) => {
        console.error('Error creating store', error);
        this.showSnackBar('Error creating store', 'Close');
      }
    });
  }

  updateStore(id: string, storeData: FormData): void {
    this.storeService.updateStore(id, storeData).subscribe({
      next: () => {
        this.showSnackBar('Store updated successfully', 'Close');
        this.loadStores();
      },
      error: (error) => {
        console.error('Error updating store', error);
        this.showSnackBar('Error updating store', 'Close');
      }
    });
  }

  deleteStore(id: string): void {
    if (confirm('Are you sure you want to delete this store?')) {
      this.storeService.deleteStore(id).subscribe({
        next: () => {
          this.showSnackBar('Store deleted successfully', 'Close');
          this.loadStores();
        },
        error: (error) => {
          console.error('Error deleting store', error);
          this.showSnackBar('Error deleting store', 'Close');
        }
      });
    }
  }

  showSnackBar(message: string, action: string): void {
    this.snackBar.open(message, action, {
      duration: 3000,
      horizontalPosition: 'end',
      verticalPosition: 'top'
    });
  }
}
