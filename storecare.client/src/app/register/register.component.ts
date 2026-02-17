import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, RegisterRequest } from '../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit, OnDestroy {
  currentStep = 1;
  totalSteps = 3;

  // Forms for each step
  personalInfoForm!: FormGroup;
  accountInfoForm!: FormGroup;
  verificationForm!: FormGroup;

  isSubmitting = false;
  showSuccess = false;
  showPassword = false;
  showConfirmPassword = false;
  errorMessage: string = '';

  // Password strength
  passwordStrength = 0;
  passwordStrengthText = '';
  passwordStrengthClass = '';

  // Password validation flags
  hasMinLength = false;
  hasLowerCase = false;
  hasUpperCase = false;
  hasNumberOrSpecial = false;

  // OTP simulation
  otpDigits: string[] = ['', '', '', '', '', ''];
  otpTimer = 60;
  canResend = false;
  otpInterval: any;
  showOtpVerification = false; // Set to false if you don't want OTP

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.initializeForms();
  }

  ngOnDestroy(): void {
    if (this.otpInterval) {
      clearInterval(this.otpInterval);
    }
  }

  initializeForms(): void {
    // Step 1: Personal Information
    this.personalInfoForm = this.fb.group({
      fullName: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(200),
        Validators.pattern('^[a-zA-Z\\s]*$')
      ]],
      email: ['', [
        Validators.required,
        Validators.email,
        Validators.maxLength(100)
      ]],
      phone: ['', [
        Validators.required,
        Validators.pattern('^[0-9]{10}$')
      ]]
    });

    // Step 2: Account Information
    this.accountInfoForm = this.fb.group({
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern('^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$')
      ]],
      confirmPassword: ['', Validators.required]
    }, {
      validators: this.passwordMatchValidator
    });

    // Step 3: Verification (optional)
    this.verificationForm = this.fb.group({
      otp: ['', this.showOtpVerification ? [Validators.required, Validators.minLength(6)] : []]
    });

    // Subscribe to password changes for strength meter and validation flags
    this.accountInfoForm.get('password')?.valueChanges.subscribe(password => {
      this.calculatePasswordStrength(password);
      this.updatePasswordValidationFlags(password);
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

  updatePasswordValidationFlags(password: string): void {
    if (!password) {
      this.hasMinLength = false;
      this.hasLowerCase = false;
      this.hasUpperCase = false;
      this.hasNumberOrSpecial = false;
      return;
    }

    this.hasMinLength = password.length >= 8;
    this.hasLowerCase = /[a-z]/.test(password);
    this.hasUpperCase = /[A-Z]/.test(password);
    this.hasNumberOrSpecial = /[\d@$!%*?&]/.test(password);
  }

  calculatePasswordStrength(password: string): void {
    if (!password) {
      this.passwordStrength = 0;
      this.passwordStrengthText = '';
      this.passwordStrengthClass = '';
      return;
    }

    let strength = 0;

    // Length check
    if (password.length >= 8) strength += 25;

    // Contains lowercase
    if (/[a-z]/.test(password)) strength += 25;

    // Contains uppercase
    if (/[A-Z]/.test(password)) strength += 25;

    // Contains number or special character
    if (/[\d@$!%*?&]/.test(password)) strength += 25;

    this.passwordStrength = strength;

    if (strength < 50) {
      this.passwordStrengthText = 'Weak';
      this.passwordStrengthClass = 'weak';
    } else if (strength < 75) {
      this.passwordStrengthText = 'Medium';
      this.passwordStrengthClass = 'medium';
    } else {
      this.passwordStrengthText = 'Strong';
      this.passwordStrengthClass = 'strong';
    }
  }

  // Navigation methods
  nextStep(): void {
    if (this.currentStep === 1 && this.personalInfoForm.valid) {
      this.currentStep = 2;
      window.scrollTo(0, 0);
    } else if (this.currentStep === 2 && this.accountInfoForm.valid) {
      if (this.showOtpVerification) {
        this.currentStep = 3;
        this.startOtpTimer();
        // In real app, send OTP to email/phone here
        console.log('OTP sent to:', this.personalInfoForm.value.email);
      } else {
        // Skip OTP and register directly
        this.onSubmit();
      }
      window.scrollTo(0, 0);
    }
  }

  previousStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
      window.scrollTo(0, 0);
    }
  }

  goToStep(step: number): void {
    if (step < this.currentStep) {
      this.currentStep = step;
    } else if (step === 2 && this.personalInfoForm.valid) {
      this.currentStep = 2;
    } else if (step === 3 && this.personalInfoForm.valid && this.accountInfoForm.valid && this.showOtpVerification) {
      this.currentStep = 3;
    }
  }

  // Form validation helpers
  isFieldInvalid(form: FormGroup, fieldName: string): boolean {
    const field = form.get(fieldName);
    return field ? (field.invalid && (field.dirty || field.touched)) : false;
  }

  getFieldError(form: FormGroup, fieldName: string): string {
    const field = form.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return 'This field is required';
      if (field.errors['email']) return 'Please enter a valid email';
      if (field.errors['minlength']) return `Minimum ${field.errors['minlength'].requiredLength} characters required`;
      if (field.errors['pattern']) {
        if (fieldName === 'fullName') return 'Only letters and spaces allowed';
        if (fieldName === 'phone') return 'Enter a valid 10-digit phone number';
        if (fieldName === 'password') return 'Must contain uppercase, lowercase, number and special character';
      }
      if (field.errors['mismatch']) return 'Passwords do not match';
    }
    return '';
  }

  // Password visibility toggle
  togglePasswordVisibility(field: string): void {
    if (field === 'password') {
      this.showPassword = !this.showPassword;
    } else if (field === 'confirm') {
      this.showConfirmPassword = !this.showConfirmPassword;
    }
  }

  // OTP Methods
  onOtpInput(event: any, index: number): void {
    const input = event.target;
    const value = input.value;

    if (value && index < 5) {
      const nextInput = document.getElementById(`otp-${index + 1}`);
      nextInput?.focus();
    }

    this.otpDigits[index] = value;
    this.verificationForm.patchValue({ otp: this.otpDigits.join('') });
  }

  onOtpKeyDown(event: KeyboardEvent, index: number): void {
    if (event.key === 'Backspace' && !this.otpDigits[index] && index > 0) {
      const prevInput = document.getElementById(`otp-${index - 1}`);
      prevInput?.focus();
    }
  }

  startOtpTimer(): void {
    this.otpTimer = 60;
    this.canResend = false;

    this.otpInterval = setInterval(() => {
      this.otpTimer--;
      if (this.otpTimer <= 0) {
        clearInterval(this.otpInterval);
        this.canResend = true;
      }
    }, 1000);
  }

  resendOtp(): void {
    if (this.canResend) {
      console.log('Resending OTP to:', this.personalInfoForm.value.email);
      this.startOtpTimer();
    }
  }

  // Submit registration
  onSubmit(): void {
    // If OTP is enabled, verify step 3
    if (this.showOtpVerification && this.currentStep === 3 && !this.verificationForm.valid) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const registerData: RegisterRequest = {
      fullName: this.personalInfoForm.value.fullName,
      email: this.personalInfoForm.value.email,
      phone: this.personalInfoForm.value.phone,
      password: this.accountInfoForm.value.password,
      confirmPassword: this.accountInfoForm.value.confirmPassword
    };

    this.authService.register(registerData).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        this.showSuccess = true;

        // Clear OTP timer
        if (this.otpInterval) {
          clearInterval(this.otpInterval);
        }

        // Redirect to login after 3 seconds
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 3000);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = error.message;
        console.error('Registration failed:', error);

        // Scroll to top to show error
        window.scrollTo(0, 0);
      }
    });
  }

  // Go to login
  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
