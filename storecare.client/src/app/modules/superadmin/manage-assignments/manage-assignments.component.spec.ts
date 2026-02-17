import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageAssignmentsComponent } from './manage-assignments.component';

describe('ManageAssignmentsComponent', () => {
  let component: ManageAssignmentsComponent;
  let fixture: ComponentFixture<ManageAssignmentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ManageAssignmentsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageAssignmentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
