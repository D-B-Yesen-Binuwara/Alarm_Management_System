import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Alarm } from '../models/alarm.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AlarmService {
  private readonly url = `${environment.apiUrl}/alarm`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Alarm[]> {
    return this.http.get<Alarm[]>(this.url).pipe(
      catchError(() => of([]))
    );
  }

  getActive(): Observable<Alarm[]> {
    return this.http.get<Alarm[]>(`${this.url}/active`).pipe(
      catchError(() => of([]))
    );
  }

  getByDevice(deviceId: number): Observable<Alarm[]> {
    return this.http.get<Alarm[]>(`${this.url}/device/${deviceId}`).pipe(
      catchError(() => of([]))
    );
  }
}
