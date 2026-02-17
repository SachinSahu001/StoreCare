import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Store } from '../../../../services/store.service';

@Component({
  selector: 'app-store-dialog',
  standalone: false,
  templateUrl: './store-dialog.component.html',
  styleUrl: './store-dialog.component.css'
})
export class StoreDialogComponent {
  storeForm: FormGroup;
  isEditMode: boolean;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<StoreDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Partial<Store>
  ) {
    this.isEditMode = !!data.id;

    this.storeForm = this.fb.group({
      storeName: [data.storeName || '', Validators.required],
      storeCode: [data.storeCode || '', Validators.required],
      storeEmail: [data.storeEmail || '', [Validators.required, Validators.email]],
      storePhone: [data.storePhone || '', Validators.required],
      storeAddress: [data.storeAddress || '', Validators.required],
      city: [data.city || '', Validators.required],
      state: [data.state || '', Validators.required],
      pincode: [data.pincode || '', Validators.required],
      gstNumber: [data.gstNumber || ''],
      active: [data.active !== undefined ? data.active : true]
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.storeForm.valid) {
      this.dialogRef.close(this.storeForm.value);
    }
  }
}
