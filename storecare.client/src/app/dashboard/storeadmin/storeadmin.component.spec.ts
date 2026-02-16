import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StoreadminComponent } from './storeadmin.component';

describe('StoreadminComponent', () => {
  let component: StoreadminComponent;
  let fixture: ComponentFixture<StoreadminComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [StoreadminComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StoreadminComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
