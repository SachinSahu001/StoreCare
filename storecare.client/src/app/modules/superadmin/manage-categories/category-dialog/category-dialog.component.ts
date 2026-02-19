import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ProductCategory } from '../../../../core/services/product.service';

@Component({
  selector: 'app-category-dialog',
  standalone: false,
  templateUrl: './category-dialog.component.html',
  styleUrls: ['./category-dialog.component.css']
})
export class CategoryDialogComponent implements OnInit {
  categoryForm: FormGroup;
  isEditMode: boolean;
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<CategoryDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Partial<ProductCategory>
  ) {
    this.isEditMode = !!data.id;
    this.imagePreview = data.imageUrl || null;

    this.categoryForm = this.fb.group({
      categoryName: [data.categoryName || '', [Validators.required, Validators.minLength(3)]],
      categoryDescription: [data.categoryDescription || '', Validators.maxLength(500)],
      displayOrder: [data.displayOrder || 0, [Validators.required, Validators.min(0)]],
      active: [data.active !== undefined ? data.active : true]
    });
  }

  ngOnInit(): void {
    // Optional: Add listeners or specific initialization if needed
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];

      // Basic Validation
      if (!file.type.startsWith('image/')) {
        alert('Please select an image file');
        return;
      }

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
    if (this.categoryForm.valid) {
      const formValue = this.categoryForm.getRawValue();
      this.dialogRef.close({
        ...formValue,
        file: this.selectedFile
      });
    } else {
      this.categoryForm.markAllAsTouched();
    }
  }

  // Helper to check errors
  isFieldInvalid(fieldName: string): boolean {
    const field = this.categoryForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }
}
