import { Component, OnInit, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Network } from 'vis-network/standalone';

@Component({
  selector: 'app-graph',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './graph.html',
  styleUrls: ['./graph.css']
})
export class GraphComponent implements OnInit {

  constructor(private el: ElementRef) { }

  ngOnInit() {
    this.loadGraph();
  }

  loadGraph() {

    const container = this.el.nativeElement.querySelector('#network');

    const nodes = [
      { id: 1, label: 'Router' },
      { id: 2, label: 'Switch 1' },
      { id: 3, label: 'Switch 2' },
      { id: 4, label: 'Server' }
    ];

    const edges = [
      { from: 1, to: 2 },
      { from: 1, to: 3 },
      { from: 2, to: 4 }
    ];

    const data: any = {
      nodes: nodes,
      edges: edges
    };

    const options: any = {

      nodes: {
        shape: 'dot',
        size: 20,
        font: {
          size: 14
        }
      },

      edges: {
        width: 2
      },

      physics: {
        enabled: true
      }

    };

    new Network(container, data, options);

  }

}
