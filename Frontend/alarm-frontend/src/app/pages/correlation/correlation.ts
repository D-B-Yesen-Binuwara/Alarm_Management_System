import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-correlation',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './correlation.html',
  styleUrls: ['./correlation.css']
})
export class CorrelationComponent implements OnInit {

  deviceId: number = 1;

  devices: any[] = [];

  alarmDevice: string = "-";
  rootDevice: string = "-";
  path: string = "-";
  status: string = "-";
  severity: string = "-";

  pathDevices: string[] = [];

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.loadDevices();
  }

  loadDevices() {

    this.http.get<any[]>('http://localhost:5289/api/device')
      .subscribe((data: any[]) => {

        this.devices = data;

      });

  }

  runCorrelation() {

    this.http.get<any>('http://localhost:5289/api/Correlation/' + this.deviceId)
      .subscribe({

        next: (data: any) => {

          this.alarmDevice = data.alarmDevice;
          this.rootDevice = data.rootCauseDevice;

          this.pathDevices = data.path;
          this.path = data.path.join(" → ");

          this.status = data.status;
          this.severity = data.severity;

        },

        error: () => {

          alert("Correlation API failed. Check backend server.");

        }

      });

  }

}
