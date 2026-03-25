import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-alarm',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './alarm.html',
  styleUrls: ['./alarm.css']
})
export class AlarmComponent implements OnInit {

  alarms: any[] = [];

  constructor(private http: HttpClient) { }

  ngOnInit() {

    this.loadAlarms();

    // refresh every 5 seconds
    setInterval(() => {

      this.loadAlarms();

    }, 5000);

  }

  loadAlarms() {

    this.http.get<any[]>('http://localhost:5289/api/alarm')
      .subscribe((data: any[]) => {

        this.alarms = data;

      });

  }

}
