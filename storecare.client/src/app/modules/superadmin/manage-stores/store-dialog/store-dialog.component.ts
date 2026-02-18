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
      storeCode: [{ value: data.storeCode || 'Auto-generated', disabled: true }],
      email: [data.email || '', [Validators.required, Validators.email]], // Changed from storeEmail
      contactNumber: [data.contactNumber || '', [Validators.required, Validators.pattern('^[0-9]{10}$')]], // Changed from storePhone
      address: [data.address || '', Validators.required], // Changed from storeAddress
      active: [data.isActive !== undefined ? data.isActive : true] // Changed from active/data.active
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
