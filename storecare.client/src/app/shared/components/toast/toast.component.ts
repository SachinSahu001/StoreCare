import { Component, OnInit } from '@angular/core';
import { ToastService, Toast } from '../../../services/toast.service';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
    selector: 'app-toast',
    standalone: false,
    template: `
    <div class="toast-container">
      <div *ngFor="let toast of toasts" 
           [@fadeSlide]
           class="toast" 
           [ngClass]="toast.type"
           (click)="remove(toast.id)">
        <div class="toast-icon">
          <i class="fas" [ngClass]="getIcon(toast.type)"></i>
        </div>
        <div class="toast-message">{{ toast.message }}</div>
        <div class="toast-close">
          <i class="fas fa-times"></i>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .toast-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 10px;
      max-width: 350px;
      width: 100%;
      pointer-events: none;
    }

    .toast {
      pointer-events: auto;
      background: white;
      padding: 1rem;
      border-radius: 8px; /* Standardize with var if available */
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      display: flex;
      align-items: center;
      gap: 12px;
      cursor: pointer;
      overflow: hidden;
      border-left: 4px solid #ccc;
      min-height: 50px;
      font-size: 0.9rem;
    }

    .toast-icon {
      font-size: 1.2rem;
    }

    .toast-message {
      flex: 1;
      line-height: 1.4;
    }

    .toast-close {
      color: #999;
      font-size: 0.9rem;
    }

    /* Types */
    .toast.success { border-left-color: #22c55e; }
    .toast.success .toast-icon { color: #22c55e; }

    .toast.error { border-left-color: #ef4444; }
    .toast.error .toast-icon { color: #ef4444; }

    .toast.info { border-left-color: #3b82f6; }
    .toast.info .toast-icon { color: #3b82f6; }

    .toast.warning { border-left-color: #f59e0b; }
    .toast.warning .toast-icon { color: #f59e0b; }
  `],
    animations: [
        trigger('fadeSlide', [
            state('void', style({ opacity: 0, transform: 'translateY(-20px)' })),
            transition(':enter', [
                animate('300ms ease-out')
            ]),
            transition(':leave', [
                animate('200ms ease-in', style({ opacity: 0 }))
            ])
        ])
    ]
})
export class ToastComponent implements OnInit {
    toasts: Toast[] = [];

    constructor(private toastService: ToastService) { }

    ngOnInit(): void {
        this.toastService.toasts$.subscribe(toasts => {
            this.toasts = toasts;
        });
    }

    remove(id: string) {
        this.toastService.remove(id);
    }

    getIcon(type: string): string {
        switch (type) {
            case 'success': return 'fa-check-circle';
            case 'error': return 'fa-exclamation-circle';
            case 'info': return 'fa-info-circle';
            case 'warning': return 'fa-exclamation-triangle';
            default: return 'fa-info-circle';
        }
    }
}
