import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StoreAdminDialogComponent } from './store-admin-dialog.component';

describe('StoreAdminDialogComponent', () => {
  let component: StoreAdminDialogComponent;
  let fixture: ComponentFixture<StoreAdminDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StoreAdminDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StoreAdminDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
