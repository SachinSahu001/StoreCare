import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AuthService, RegisterRequest } from '../../../../services/auth.service';
import { ProductService, ProductCategory } from '../../../../services/product.service';
import { StoreService } from '../../../../services/store.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SelectionModel } from '@angular/cdk/collections';

@Component({
  selector: 'app-store-admin-dialog',
  standalone: false,
  templateUrl: './store-admin-dialog.component.html',
  styleUrl: './store-admin-dialog.component.css'
})
export class StoreAdminDialogComponent implements OnInit {
  // Steps: 0 = User Details, 1 = Store Details, 2 = Product Assignment
  currentStep = 0;

  adminForm: FormGroup;
  storeForm: FormGroup;
  isLoading = false;
  selectedLogo: File | null = null;
  logoPreview: string | null = null;

  // Assignment Data
  categories: ProductCategory[] = [];
  categorySelection = new SelectionModel<string>(true, []);

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<StoreAdminDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private authService: AuthService,
    private productService: ProductService,
    private storeService: StoreService,
    private snackBar: MatSnackBar
  ) {
    this.adminForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });

    this.storeForm = this.fb.group({
      storeName: ['', Validators.required],
      contactNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      email: ['', [Validators.required, Validators.email]],
      address: ['', Validators.required],
      city: [''],
      state: [''],
      pincode: ['']
    });
  }

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.productService.getCategories().subscribe(data => this.categories = data);
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  onLogoSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedLogo = file;
      const reader = new FileReader();
      reader.onload = () => {
        this.logoPreview = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  nextStep(): void {
    if (this.currentStep === 0 && this.adminForm.valid) {
      // Pre-fill store details from admin details if empty
      if (!this.storeForm.value.storeName) {
        this.storeForm.patchValue({
          storeName: `${this.adminForm.value.fullName}'s Store`,
          contactNumber: this.adminForm.value.phone,
          email: this.adminForm.value.email
        });
      }
      this.currentStep = 1;
    } else if (this.currentStep === 1 && this.storeForm.valid) {
      this.currentStep = 2;
    }
  }

  prevStep(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  toggleCategory(id: string): void {
    this.categorySelection.toggle(id);
  }

  onSave(): void {
    if (this.adminForm.invalid || this.storeForm.invalid) return;

    this.isLoading = true;
    const request: RegisterRequest = {
      fullName: this.adminForm.value.fullName,
      email: this.adminForm.value.email,
      phone: this.adminForm.value.phone,
      password: this.adminForm.value.password,
      confirmPassword: this.adminForm.value.confirmPassword
    };

    // 1. Create StoreAdmin (and initial Store)
    this.authService.createStoreAdmin(request).subscribe({
      next: (res) => {
        const storeId = res.storeId;

        // 2. Update Store Details
        const storeFormData = new FormData();
        storeFormData.append('StoreName', this.storeForm.value.storeName);
        storeFormData.append('ContactNumber', this.storeForm.value.contactNumber);
        storeFormData.append('Email', this.storeForm.value.email);
        storeFormData.append('Address', this.storeForm.value.address);
        // Add optional fields if backend supports them, or concatenate to address
        // For now sticking to core DTO fields I verified earlier

        if (this.selectedLogo) {
          storeFormData.append('StoreLogo', this.selectedLogo);
        }

        this.storeService.updateStore(storeId, storeFormData).subscribe({
          next: () => {
            // 3. Assign Products
            if (this.categorySelection.selected.length > 0) {
              this.assignProducts(storeId, this.storeForm.value.storeName);
            } else {
              this.finish(this.storeForm.value.storeName);
            }
          },
          error: (err) => {
            console.error('Error updating store details', err);
            this.snackBar.open('Admin created, but failed to update store details.', 'Close', { duration: 5000 });
            this.isLoading = false;
          }
        });
      },
      error: (error) => {
        console.error('Error creating admin', error);
        this.snackBar.open('Error: ' + (error.error?.message || 'Unknown error'), 'Close', { duration: 5000 });
        this.isLoading = false;
      }
    });
  }

  assignProducts(storeId: string, storeName: string): void {
    const categoryIds = this.categorySelection.selected;
    if (categoryIds.length === 0) {
      this.finish(storeName);
      return;
    }

    this.processAssignment(storeId, [...categoryIds], storeName);
  }

  processAssignment(storeId: string, categoryIds: string[], storeName: string) {
    if (categoryIds.length === 0) {
      this.finish(storeName);
      return;
    }

    const catId = categoryIds.pop();
    if (!catId) return;

    this.productService.getProductsByCategoryForAssignment(catId, storeId).subscribe({
      next: (res) => {
        const products = res.data || [];
        const productIds = products.map((p: any) => p.id);

        if (productIds.length > 0) {
          const payload = {
            storeId,
            categoryId: catId,
            productIds,
            canManage: true
          };
          this.productService.assignProductsByCategory(payload).subscribe({
            next: () => this.processAssignment(storeId, categoryIds, storeName),
            error: () => this.processAssignment(storeId, categoryIds, storeName)
          });
        } else {
          this.processAssignment(storeId, categoryIds, storeName);
        }
      },
      error: () => this.processAssignment(storeId, categoryIds, storeName)
    });
  }

  finish(storeName: string): void {
    this.snackBar.open(`Store Admin for "${storeName}" created successfully`, 'Close', { duration: 3000 });
    this.isLoading = false;
    this.dialogRef.close(true);
  }
}
