import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaymentHistoryModalComponent } from './payment-history-modal';
import { IronLogicApiService } from '@core/services/iron-logic-api.service';
import { of } from 'rxjs';
import { DebugElement } from '@angular/core';

describe('PaymentHistoryModalComponent', () => {
  let component: PaymentHistoryModalComponent;
  let fixture: ComponentFixture<PaymentHistoryModalComponent>;
  let apiService: jasmine.SpyObj<IronLogicApiService>;

  beforeEach(async () => {
    const apiSpy = jasmine.createSpyObj('IronLogicApiService', ['getUserTransactions']);

    await TestBed.configureTestingModule({
      imports: [PaymentHistoryModalComponent],
      providers: [{ provide: IronLogicApiService, useValue: apiSpy }],
    }).compileComponents();

    apiService = TestBed.inject(IronLogicApiService) as jasmine.SpyObj<IronLogicApiService>;
    fixture = TestBed.createComponent(PaymentHistoryModalComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not load transactions when modal is closed', () => {
    component.isOpen = false;
    component.userId = 'USR-123';
    fixture.detectChanges();
    expect(apiService.getUserTransactions).not.toHaveBeenCalled();
  });

  it('should fetch transactions when modal opens with a userId', async () => {
    const mockTransactions = [
      {
        id: 'TXN-001',
        date: new Date('2026-04-09'),
        amount: 9.99,
        planType: 'Pro',
        status: 'Paid',
        invoiceNumber: 'INV-2026-001',
      },
    ];

    apiService.getUserTransactions.and.returnValue(of(mockTransactions));

    component.isOpen = true;
    component.userId = 'USR-123';
    component.userName = 'Test User';

    fixture.detectChanges();
    await fixture.whenStable();

    expect(apiService.getUserTransactions).toHaveBeenCalledWith('USR-123');
    expect(component.transactions()).toEqual(mockTransactions);
  });

  it('should emit close event when onClose is called', () => {
    spyOn(component.close, 'emit');
    component.onClose();
    expect(component.close.emit).toHaveBeenCalled();
  });

  it('should emit close and navigate when viewing full details', () => {
    const router = TestBed.inject(Router);
    spyOn(router, 'navigate');
    spyOn(component.close, 'emit');

    component.userId = 'USR-123';
    component.userName = 'Test User';
    component.onViewFullDetails();

    expect(component.close.emit).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/admin/financial'], {
      queryParams: {
        search: 'Test User',
      },
    });
  });

  it('should return correct status classes', () => {
    expect(component.getStatusClasses('Paid')).toContain('emerald');
    expect(component.getStatusClasses('Pending')).toContain('amber');
    expect(component.getStatusClasses('Failed')).toContain('rose');
  });

  it('should format currency correctly', () => {
    const formatted = component.formatCurrency(100);
    expect(formatted).toContain('100');
  });

  it('should display no transactions message when empty', () => {
    component.isOpen = true;
    component.transactions.set([]);
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('No transactions found');
  });
});

import { Router } from '@angular/router';
