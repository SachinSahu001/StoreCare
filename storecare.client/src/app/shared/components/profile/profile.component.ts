import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService, UserProfile } from '../../../services/auth.service';

@Component({
    selector: 'app-profile',
    standalone: false,
    templateUrl: './profile.component.html',
    styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
    profileForm: FormGroup;
    passwordForm: FormGroup;
    user: UserProfile | null = null;
    isLoading = false;
    loginHistory: any[] = [];
    historyColumns: string[] = ['loginTime', 'ipAddress', 'browser', 'status', 'duration'];

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private snackBar: MatSnackBar
    ) {
        this.profileForm = this.fb.group({
            fullName: ['', Validators.required],
            email: [{ value: '', disabled: true }], // Email cannot be changed
            phone: ['', [Validators.pattern('^[0-9]{10}$')]],
            role: [{ value: '', disabled: true }],
            storeName: [{ value: '', disabled: true }]
        });

        this.passwordForm = this.fb.group({
            currentPassword: ['', Validators.required],
            newPassword: ['', [Validators.required, Validators.minLength(8)]],
            confirmNewPassword: ['', Validators.required]
        }, { validators: this.passwordMatchValidator });
    }

    ngOnInit(): void {
        this.loadProfile();
        this.loadLoginHistory();
    }

    loadProfile(): void {
        this.isLoading = true;
        this.authService.getProfile().subscribe({
            next: (user) => {
                this.user = user;
                this.profileForm.patchValue({
                    fullName: user.fullName,
                    email: user.email,
                    phone: user.phone,
                    role: user.role,
                    storeName: user.storeName || 'N/A'
                });
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error loading profile', err);
                this.isLoading = false;
            }
        });
    }

    loadLoginHistory(): void {
        this.authService.getMyLoginHistory().subscribe({
            next: (history: any[]) => {
                this.loginHistory = history;
            },
            error: (err: HttpErrorResponse) => console.error('Error loading history', err)
        });
    }

    passwordMatchValidator(g: FormGroup) {
        return g.get('newPassword')?.value === g.get('confirmNewPassword')?.value
            ? null : { mismatch: true };
    }

    updateProfile(): void {
        if (this.profileForm.invalid) return;

        this.isLoading = true;
        this.authService.updateProfile(this.profileForm.value).subscribe({
            next: (updatedUser: UserProfile) => {
                this.user = updatedUser;
                this.snackBar.open('Profile updated successfully', 'Close', { duration: 3000 });
                this.isLoading = false;
            },
            error: (err: HttpErrorResponse) => {
                console.error('Error updating profile', err);
                this.snackBar.open('Failed to update profile', 'Close', { duration: 3000 });
                this.isLoading = false;
            }
        });
    }

    changePassword(): void {
        if (this.passwordForm.invalid) return;

        this.isLoading = true;
        const { currentPassword, newPassword, confirmNewPassword } = this.passwordForm.value;

        this.authService.changePassword({ currentPassword, newPassword, confirmNewPassword }).subscribe({
            next: () => {
                this.snackBar.open('Password changed successfully', 'Close', { duration: 3000 });
                this.passwordForm.reset();
                this.isLoading = false;
            },
            error: (err: HttpErrorResponse) => {
                this.snackBar.open(err.error?.message || 'Failed to change password', 'Close', { duration: 3000 });
                this.isLoading = false;
            }
        });
    }

    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (input.files && input.files[0]) {
            const file = input.files[0];
            const formData = new FormData();
            formData.append('ProfileImage', file);

            this.isLoading = true;
            this.authService.uploadProfilePicture(formData).subscribe({
                next: (res: any) => {
                    if (this.user) {
                        this.user.profilePictureUrl = res.profilePictureUrl;
                        this.authService.getProfile().subscribe(); // Refresh global state
                    }
                    this.snackBar.open('Profile picture updated', 'Close', { duration: 3000 });
                    this.isLoading = false;
                },
                error: (err: HttpErrorResponse) => {
                    this.snackBar.open('Failed to upload picture', 'Close', { duration: 3000 });
                    this.isLoading = false;
                }
            });
        }
    }
}
