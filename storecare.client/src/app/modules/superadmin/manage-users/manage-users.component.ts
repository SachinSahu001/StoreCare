import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatSnackBar } from '@angular/material/snack-bar';
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
    private snackBar: MatSnackBar
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
}
