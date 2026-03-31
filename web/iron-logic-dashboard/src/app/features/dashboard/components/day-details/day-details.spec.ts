import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DayDetails } from './day-details';

describe('DayDetails', () => {
  let component: DayDetails;
  let fixture: ComponentFixture<DayDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DayDetails],
    }).compileComponents();

    fixture = TestBed.createComponent(DayDetails);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
