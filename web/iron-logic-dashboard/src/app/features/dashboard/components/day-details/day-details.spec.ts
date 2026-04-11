import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DayDetailsComponent } from './day-details';

describe('DayDetailsComponent', () => {
  let component: DayDetailsComponent;
  let fixture: ComponentFixture<DayDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DayDetailsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DayDetailsComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
