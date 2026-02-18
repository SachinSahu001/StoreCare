import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Product, ProductCategory } from '../../../../services/product.service';

interface DialogData {
  product: Partial<Product>;
  categories: ProductCategory[];
}

@Component({
  selector: 'app-product-dialog',
  standalone: false,
  templateUrl: './product-dialog.component.html',
  styleUrl: './product-dialog.component.css'
})
export class ProductDialogComponent {
  productForm: FormGroup;
  isEditMode: boolean;
  categories: ProductCategory[];
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<ProductDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {
    this.isEditMode = !!data.product.id;
    this.categories = data.categories;
    this.imagePreview = data.product.imageUrl || null;

    this.productForm = this.fb.group({
      productName: [data.product.productName || '', Validators.required],
      productCode: [{ value: data.product.productCode || 'Auto-generated', disabled: true }],
      categoryId: [data.product.categoryId || '', Validators.required],
      brandName: [data.product.brand || '', Validators.required], // Backend expects BrandName, mapped from brand
      model: [data.product.model || ''],
      productDescription: [data.product.productDescription || ''],
      price: [data.product.price || 0, [Validators.required, Validators.min(0)]],
      mrp: [data.product.mrp || 0, [Validators.min(0)]],
      stockQuantity: [data.product.stockQuantity || 0, [Validators.required, Validators.min(0)]],
      unit: [data.product.unit || 'PCS', Validators.required],
      isFeatured: [data.product.isFeatured || false],
      active: [data.product.active !== undefined ? data.product.active : true]
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;

      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.productForm.valid) {
      const formValue = this.productForm.getRawValue();
      this.dialogRef.close({ ...formValue, file: this.selectedFile });
    }
  }
}
