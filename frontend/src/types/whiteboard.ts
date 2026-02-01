export type ViewMode = 'grid' | 'whiteboard';

export interface Position {
  x: number;
  y: number;
}

export interface Viewport {
  x: number;
  y: number;
  zoom: number;
}

export interface WhiteboardState {
  positions: Record<string, Position>;
  viewport: Viewport;
  viewMode: ViewMode;
}
