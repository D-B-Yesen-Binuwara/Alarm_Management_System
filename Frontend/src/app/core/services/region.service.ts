import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Region } from '../models/region.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class RegionService {
  private readonly url = `${environment.apiUrl}/region`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Region[]> {
    return this.http.get<Region[]>(this.url);
  }

  getById(id: number): Observable<Region> {
    return this.http.get<Region>(`${this.url}/${id}`);
  }
}
