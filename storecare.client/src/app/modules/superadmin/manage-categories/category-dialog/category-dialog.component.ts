import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ProductCategory } from '../../../../services/product.service';

@Component({
  selector: 'app-category-dialog',
  standalone: false,
  templateUrl: './category-dialog.component.html',
  styleUrl: './category-dialog.component.css'
})
export class CategoryDialogComponent {
  categoryForm: FormGroup;
  isEditMode: boolean;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<CategoryDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Partial<ProductCategory>
  ) {
    this.isEditMode = !!data.id;
    this.categoryForm = this.fb.group({
      categoryName: [data.categoryName || '', Validators.required],
      categoryCode: [data.categoryCode || '', Validators.required],
      description: [data.description || ''],
      imageUrl: [data.imageUrl || ''],
      displayOrder: [data.displayOrder || 0, [Validators.required, Validators.min(0)]],
      active: [data.active !== undefined ? data.active : true]
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.categoryForm.valid) {
      this.dialogRef.close(this.categoryForm.value);
    }
  }
}
