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
  selectedFile: File | null = null;
  imagePreview: string | null = null; // To show preview of selected file or existing URL

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<CategoryDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Partial<ProductCategory>
  ) {
    this.isEditMode = !!data.id;
    this.imagePreview = data.imageUrl || null;

    this.categoryForm = this.fb.group({
      categoryName: [data.categoryName || '', Validators.required],
      categoryCode: [{ value: data.categoryCode || 'Auto-generated', disabled: true }], // Always disabled
      categoryDescription: [data.categoryDescription || ''],
      displayOrder: [data.displayOrder || 0, [Validators.required, Validators.min(0)]],
      active: [data.active !== undefined ? data.active : true]
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;

      // Create preview
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
      // Return form value + selected file
      // We need to use getRawValue() to include disabled fields if we needed them, 
      // but for categoryCode we might not need to send it on create, and on edit it's in the URL/ID.
      // However, usually we return what the form has.
      const formValue = this.categoryForm.getRawValue();
      this.dialogRef.close({ ...formValue, file: this.selectedFile });
    }
  }
}
