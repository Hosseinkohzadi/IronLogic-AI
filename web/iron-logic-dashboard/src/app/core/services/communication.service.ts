import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';

export type EmailDeliveryStatus = 'Sent' | 'Failed' | 'Queued';

export interface UserEmailHistoryItem {
  id: string;
  subject: string;
  sentAt: string;
  status: EmailDeliveryStatus;
}

export interface SendManualEmailRequest {
  subject: string;
  message: string;
}

@Injectable({
  providedIn: 'root',
})
export class CommunicationService {
  private readonly http = inject(HttpClient);
  private readonly communicationUrl = `${environment.apiUrl}/Communications`;

  getHistory(userId: string): Observable<UserEmailHistoryItem[]> {
    return this.http.get<UserEmailHistoryItem[]>(`${this.communicationUrl}/users/${userId}/emails`);
  }

  sendEmail(userId: string, subject: string, message: string): Observable<void> {
    return this.http.post<void>(`${this.communicationUrl}/users/${userId}/emails`, {
      subject,
      message,
    });
  }

  getUserEmailHistory(userId: string): Observable<UserEmailHistoryItem[]> {
    return this.getHistory(userId);
  }

  sendManualEmail(userId: string, payload: SendManualEmailRequest): Observable<void> {
    return this.sendEmail(userId, payload.subject, payload.message);
  }
}
