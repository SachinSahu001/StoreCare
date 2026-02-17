import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AuthService, RegisterRequest } from '../../../../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

interface DialogData {
  storeId: string;
  storeName: string;
}

@Component({
  selector: 'app-store-admin-dialog',
  standalone: false,
  templateUrl: './store-admin-dialog.component.html',
  styleUrl: './store-admin-dialog.component.css'
})
export class StoreAdminDialogComponent {
  adminForm: FormGroup;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<StoreAdminDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {
    this.adminForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.adminForm.valid) {
      this.isLoading = true;
      const request: RegisterRequest = {
        fullName: this.adminForm.value.fullName,
        email: this.adminForm.value.email,
        phone: this.adminForm.value.phone,
        password: this.adminForm.value.password,
        confirmPassword: this.adminForm.value.confirmPassword,
        role: 'StoreAdmin',
        storeId: this.data.storeId
      };

      this.authService.register(request).subscribe({
        next: () => {
          this.snackBar.open('Store Admin created successfully', 'Close', { duration: 3000 });
          this.isLoading = false;
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Error creating admin', error);
          this.snackBar.open('Error creating admin: ' + (error.error?.message || 'Unknown error'), 'Close', { duration: 5000 });
          this.isLoading = false;
        }
      });
    }
  }
}
