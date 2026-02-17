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

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ProductDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData
  ) {
    this.isEditMode = !!data.product.id;
    this.categories = data.categories;

    this.productForm = this.fb.group({
      productName: [data.product.productName || '', Validators.required],
      productCode: [data.product.productCode || '', Validators.required],
      categoryId: [data.product.categoryId || '', Validators.required],
      brand: [data.product.brand || '', Validators.required],
      model: [data.product.model || ''],
      description: [data.product.description || ''],
      price: [data.product.price || 0, [Validators.required, Validators.min(0)]],
      mrp: [data.product.mrp || 0, [Validators.min(0)]],
      stockQuantity: [data.product.stockQuantity || 0, [Validators.required, Validators.min(0)]],
      unit: [data.product.unit || 'PCS', Validators.required],
      imageUrl: [data.product.imageUrl || ''],
      active: [data.product.active !== undefined ? data.product.active : true]
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.productForm.valid) {
      this.dialogRef.close(this.productForm.value);
    }
  }
}
