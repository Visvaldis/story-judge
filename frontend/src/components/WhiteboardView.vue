<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { VueFlow } from '@vue-flow/core';
import { Background } from '@vue-flow/background';
import { Controls } from '@vue-flow/controls';
import { useDebounceFn } from '@vueuse/core';
import { useStoryStore } from '../stores/story';
import { useWhiteboardStore } from '../stores/whiteboard';
import StoryNode from './StoryNode.vue';
import type { Node, NodeDragEvent, ViewportTransform } from '@vue-flow/core';

const router = useRouter();
const storyStore = useStoryStore();
const whiteboardStore = useWhiteboardStore();

const expandedNodeId = ref<string | null>(null);

// Convert stories to Vue Flow nodes
const nodes = computed<Node[]>(() => {
  return storyStore.myStories.map((story, index) => {
    const position = whiteboardStore.getPosition(story.id, index);
    return {
      id: story.id,
      type: 'story',
      position,
      data: {
        story,
        expanded: expandedNodeId.value === story.id
      },
      draggable: true
    };
  });
});

// Debounced viewport save
const debouncedSaveViewport = useDebounceFn((vp: ViewportTransform) => {
  whiteboardStore.saveViewport(vp.x, vp.y, vp.zoom);
}, 500);

function onNodeDragStop(event: NodeDragEvent) {
  const { node } = event;
  whiteboardStore.savePosition(node.id, node.position.x, node.position.y);
}

function onMoveEnd(event: { flowTransform: ViewportTransform }) {
  debouncedSaveViewport(event.flowTransform);
}


function handleExpand(nodeId: string) {
  expandedNodeId.value = nodeId;
}

function handleCollapse() {
  expandedNodeId.value = null;
}

async function handleDelete(storyId: string) {
  await storyStore.deleteStory(storyId);
  whiteboardStore.removePosition(storyId);
  if (expandedNodeId.value === storyId) {
    expandedNodeId.value = null;
  }
}

function createNewStory() {
  router.push('/stories/new');
}

// Close expanded node when clicking on canvas background
function onPaneClick() {
  expandedNodeId.value = null;
}
</script>

<template>
  <div class="whiteboard-container">
    <VueFlow
      :nodes="nodes"
      :edges="[]"
      :default-viewport="whiteboardStore.viewport"
      :min-zoom="0.3"
      :max-zoom="1.5"
      :snap-to-grid="true"
      :snap-grid="[20, 20]"
      fit-view-on-init
      @node-drag-stop="onNodeDragStop"
      @move-end="onMoveEnd"
      @pane-click="onPaneClick"
    >
      <Background pattern-color="var(--color-border)" :gap="20" />
      <Controls />

      <!-- Custom node events -->
      <template #node-story="nodeProps">
        <StoryNode
          :data="nodeProps.data"
          @expand="handleExpand(nodeProps.id)"
          @collapse="handleCollapse"
          @delete="handleDelete"
        />
      </template>
    </VueFlow>

    <!-- Empty State -->
    <div v-if="storyStore.myStories.length === 0" class="empty-state">
      <p>No stories yet</p>
      <button class="btn btn-primary" @click="createNewStory">
        + Create Your First Story
      </button>
    </div>

    <!-- Floating New Story Button -->
    <button
      v-else
      class="fab"
      @click="createNewStory"
      title="Create new story"
    >
      +
    </button>
  </div>
</template>

<style>
/* Vue Flow base styles */
@import '@vue-flow/core/dist/style.css';
@import '@vue-flow/core/dist/theme-default.css';
@import '@vue-flow/controls/dist/style.css';
</style>

<style scoped>
.whiteboard-container {
  position: relative;
  height: calc(100vh - 220px);
  min-height: 400px;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  overflow: hidden;
  background: var(--color-bg-primary);
}

.whiteboard-container :deep(.vue-flow) {
  background: var(--color-bg-primary);
}

.whiteboard-container :deep(.vue-flow__controls) {
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.whiteboard-container :deep(.vue-flow__controls-button) {
  background: var(--color-bg-secondary);
  border-bottom: 1px solid var(--color-border);
  color: var(--color-text-primary);
  fill: var(--color-text-primary);
}

.whiteboard-container :deep(.vue-flow__controls-button:hover) {
  background: var(--color-bg-hover);
}

.whiteboard-container :deep(.vue-flow__node) {
  padding: 0;
  border: none;
  background: transparent;
  border-radius: 0;
}

.empty-state {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  text-align: center;
  padding: 2rem;
  background: var(--color-bg-secondary);
  border-radius: 8px;
  border: 1px solid var(--color-border);
}

.empty-state p {
  color: var(--color-text-secondary);
  margin-bottom: 1rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  border: none;
}

.btn-primary {
  background: var(--color-accent);
  color: white;
}

.fab {
  position: absolute;
  bottom: 20px;
  right: 20px;
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: var(--color-accent);
  color: white;
  font-size: 2rem;
  border: none;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
  transition: transform 0.2s, box-shadow 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10;
}

.fab:hover {
  transform: scale(1.1);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.4);
}
</style>
