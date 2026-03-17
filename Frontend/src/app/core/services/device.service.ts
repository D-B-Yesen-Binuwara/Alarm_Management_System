import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Device } from '../models/device.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private readonly url = `${environment.apiUrl}/device`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Device[]> {
    return this.http.get<Device[]>(this.url);
  }

  getById(id: number): Observable<Device> {
    return this.http.get<Device>(`${this.url}/${id}`);
  }

  getVisibleDevices(userId: number): Observable<Device[]> {
    return this.http.get<Device[]>(`${this.url}/visible/${userId}`);
  }

  create(device: Partial<Device>): Observable<Device> {
    return this.http.post<Device>(this.url, device);
  }

  update(id: number, device: Partial<Device>): Observable<Device> {
    return this.http.put<Device>(`${this.url}/${id}`, device);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  assignDevice(id: number, userId: number): Observable<string> {
    return this.http.patch<string>(`${this.url}/${id}/assign`, { userId });
  }
}
