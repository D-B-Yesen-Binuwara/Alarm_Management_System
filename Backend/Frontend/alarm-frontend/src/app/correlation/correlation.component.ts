import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-correlation',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './correlation.component.html',
  styleUrls: ['./correlation.component.css']
})
export class CorrelationComponent {

  deviceId: number = 1;

  alarmDevice = '';
  rootDevice = '';
  path = '';
  status = '';
  severity = '';

  constructor(private http: HttpClient) { }

  runCorrelation() {

    this.http.get<any>('http://localhost:5000/api/correlation/' + this.deviceId)
      .subscribe(data => {

        this.alarmDevice = data.alarmDevice;
        this.rootDevice = data.rootCause;
        this.path = data.path.join(' -> ');
        this.status = "DOWN";
        this.severity = "CRITICAL";

      });

  }

}
