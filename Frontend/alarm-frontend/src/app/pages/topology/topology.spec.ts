import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Topology } from './topology';

describe('Topology', () => {
  let component: Topology;
  let fixture: ComponentFixture<Topology>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Topology],
    }).compileComponents();

    fixture = TestBed.createComponent(Topology);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
