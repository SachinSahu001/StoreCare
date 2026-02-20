import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { StoreAdminDialogComponent } from '../manage-stores/store-admin-dialog/store-admin-dialog.component';
import { AuthService, UserProfile } from '../../../services/auth.service';

@Component({
  selector: 'app-manage-users',
  standalone: false,
  templateUrl: './manage-users.component.html',
  styleUrl: './manage-users.component.css'
})
export class ManageUsersComponent implements OnInit {
  displayedColumns: string[] = ['profilePicture', 'fullName', 'email', 'phone', 'role', 'storeName', 'status', 'actions'];
  dataSource!: MatTableDataSource<UserProfile>;
  isLoading = true;
  selectedRole: string = '';
  roles: string[] = ['SuperAdmin', 'StoreAdmin', 'Customer'];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.authService.getUsers(this.selectedRole).subscribe({
      next: (data) => {
        this.dataSource = new MatTableDataSource(data);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading users', error);
        this.snackBar.open('Error loading users', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  onRoleChange(): void {
    this.loadUsers();
  }

  createStoreAdmin(): void {
    const dialogRef = this.dialog.open(StoreAdminDialogComponent, {
      width: '600px',
      disableClose: true,
      data: {} // No initial data needed for creation
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadUsers(); // Refresh list if created
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

  toggleStatus(user: UserProfile): void {
    const newStatus = !user.active;
    const action = newStatus ? 'activate' : 'deactivate';

    if (confirm(`Are you sure you want to ${action} this user?`)) {
      this.authService.toggleUserStatus(user.id, newStatus).subscribe({
        next: () => {
          this.snackBar.open(`User ${action}d successfully`, 'Close', { duration: 3000 });
          user.active = newStatus; // Optimistic update
        },
        error: (error) => {
          console.error(`Error ${action}ing user`, error);
          this.snackBar.open(`Error ${action}ing user`, 'Close', { duration: 3000 });
        }
      });
    }
  }

  /** Returns 1–2 uppercase initials from a full name. */
  getInitials(fullName: string): string {
    if (!fullName) return '?';
    const parts = fullName.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
  }

  /** Deterministic background color from name (no two same-initial names get the same color). */
  getAvatarColor(fullName: string): string {
    const colors = [
      '#4f46e5', '#0891b2', '#059669', '#d97706',
      '#dc2626', '#7c3aed', '#db2777', '#0284c7'
    ];
    let hash = 0;
    for (let i = 0; i < (fullName || '').length; i++) {
      hash = fullName.charCodeAt(i) + ((hash << 5) - hash);
    }
    return colors[Math.abs(hash) % colors.length];
  }

  /** When an img src 404s or errors, hide the img and show the sibling initials div. */
  onAvatarError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.style.display = 'none';
    const wrapper = img.closest('.avatar-wrapper');
    if (wrapper) {
      const fallback = wrapper.querySelector('.avatar-initials') as HTMLElement;
      if (fallback) fallback.style.display = 'flex';
    }
  }
}
