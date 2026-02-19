import { ErrorHandler, Injectable, Injector, NgZone } from '@angular/core';
import { ToastService } from '../../services/toast.service';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
    constructor(private injector: Injector, private ngZone: NgZone) { }

    handleError(error: any): void {
        const toastService = this.injector.get(ToastService);

        // Identify error type
        let message = 'An unexpected error occurred.';

        if (error instanceof HttpErrorResponse) {
            message = error.error?.message || error.message;
        } else if (error instanceof Error) {
            message = error.message;
        } else {
            message = error?.toString() || 'Unknown Error';
        }

        console.error('Global Error:', error);

        // Show toast
        this.ngZone.run(() => {
            toastService.error(message, 5000);
        });
    }
}
