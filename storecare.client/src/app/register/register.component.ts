import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: false,  // Important: set to false for non-standalone
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']  // Note: styleUrls (plural)
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  isSubmitting = false;
  showSuccess = false;
  showPassword = false;
  showConfirmPassword = false;
  showRoleField = false;
  showStoreField = false;
  selectedFileName: string = '';
  imagePreview: string | ArrayBuffer | null = null;

  constructor(
    private fb: FormBuilder,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(): void {
    this.registerForm = this.fb.group({
      userCode: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
      fullName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.pattern('^[0-9+\\-\\s]{10,15}$')]],
      roleId: [1, Validators.required],
      storeId: [''],
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern('^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$')
      ]],
      confirmPassword: ['', Validators.required],
      profilePicture: [''],
      acceptTerms: [false, Validators.requiredTrue]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (password?.value !== confirmPassword?.value) {
      confirmPassword?.setErrors({ mismatch: true });
      return { mismatch: true };
    }

    return null;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return field ? (field.invalid && (field.dirty || field.touched)) : false;
  }

  togglePasswordVisibility(field: string): void {
    if (field === 'password') {
      this.showPassword = !this.showPassword;
    } else if (field === 'confirm') {
      this.showConfirmPassword = !this.showConfirmPassword;
    }
  }

  getPasswordStrength(): number {
    const password = this.registerForm.get('password')?.value || '';
    let strength = 0;

    if (password.length >= 8) strength += 25;
    if (password.match(/[a-z]+/)) strength += 25;
    if (password.match(/[A-Z]+/)) strength += 25;
    if (password.match(/[\d]+/)) strength += 12.5;
    if (password.match(/[@$!%*?&]+/)) strength += 12.5;

    return Math.min(strength, 100);
  }

  getPasswordStrengthClass(): string {
    const strength = this.getPasswordStrength();
    if (strength < 50) return 'weak';
    if (strength < 75) return 'medium';
    return 'strong';
  }

  getPasswordStrengthText(): string {
    const strength = this.getPasswordStrength();
    if (strength < 50) return 'Weak Password';
    if (strength < 75) return 'Medium Password';
    return 'Strong Password';
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFileName = file.name;
      this.registerForm.patchValue({ profilePicture: file });

      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result;
      };
      reader.readAsDataURL(file);
    }
  }

  triggerFileInput(): void {
    document.getElementById('profilePicture')?.click();
  }

  removeImage(): void {
    this.selectedFileName = '';
    this.imagePreview = null;
    this.registerForm.patchValue({ profilePicture: '' });
    const fileInput = document.getElementById('profilePicture') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.isSubmitting = true;

      const userData = {
        id: this.generateGuid(),
        userCode: this.registerForm.value.userCode,
        fullName: this.registerForm.value.fullName,
        email: this.registerForm.value.email,
        phone: this.registerForm.value.phone || null,
        passwordHash: this.hashPassword(this.registerForm.value.password),
        roleId: this.registerForm.value.roleId,
        storeId: this.registerForm.value.storeId || null,
        profilePicture: null,
        statusId: 1,
        createdBy: this.registerForm.value.userCode,
        createdDate: new Date(),
        active: true
      };

      // Simulate API call
      setTimeout(() => {
        console.log('User registered:', userData);
        this.isSubmitting = false;
        this.showSuccess = true;
        this.registerForm.reset();
        this.removeImage();

        // Optional: Redirect after success
        // setTimeout(() => {
        //   this.router.navigate(['/login']);
        // }, 3000);
      }, 2000);
    } else {
      Object.keys(this.registerForm.controls).forEach(key => {
        const control = this.registerForm.get(key);
        control?.markAsTouched();
      });
    }
  }

  private generateGuid(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
      const r = Math.random() * 16 | 0;
      const v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }

  private hashPassword(password: string): string {
    // This is just for demo - actual hashing should be done on server
    return btoa(password);
  }
}
