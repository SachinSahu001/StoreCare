import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Validates that MRP is greater than or equal to Price
 */
export function mrpValidator(priceControlName: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        if (!control.parent) {
            return null;
        }

        const priceControl = control.parent.get(priceControlName);
        const price = priceControl?.value;
        const mrp = control.value;

        if (price && mrp && mrp < price) {
            return { mrpLessThanPrice: true };
        }
        return null;
    };
}
