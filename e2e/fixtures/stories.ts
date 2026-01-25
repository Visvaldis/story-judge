export const testStories = {
  draft: {
    title: 'Test Draft Story',
    storyType: 'Behavioral',
    situation: 'Our team faced a critical deadline for a major product launch.',
    task: 'I needed to coordinate between multiple teams and ensure all features were delivered on time.',
    action: 'I created a detailed project plan, held daily standups, and proactively identified blockers.',
    result: 'We delivered the product on time with all planned features and received positive customer feedback.',
    reflection: 'I learned the importance of early communication and risk identification.',
  },
  published: {
    title: 'Published Leadership Story',
    storyType: 'Leadership',
    situation: 'A key team member suddenly left during a critical project phase.',
    task: 'As the tech lead, I needed to ensure project continuity and team morale.',
    action: 'I redistributed tasks based on individual strengths, provided additional support, and hired a contractor.',
    result: 'The project completed successfully with only a two-week delay, and team satisfaction remained high.',
    reflection: 'This experience taught me the value of cross-training and documentation.',
  },
  technical: {
    title: 'Technical Problem Solving Story',
    storyType: 'Technical',
    situation: 'Our production system experienced intermittent performance degradation.',
    task: 'I was assigned to diagnose and resolve the root cause within 48 hours.',
    action: 'I analyzed logs, set up monitoring, identified a memory leak, and implemented a fix.',
    result: 'System performance improved by 40% and the issue never recurred.',
    reflection: 'I now advocate for better observability tools in all our services.',
  },
  conflict: {
    title: 'Conflict Resolution Story',
    storyType: 'Conflict',
    situation: 'Two senior engineers had fundamentally different approaches to system architecture.',
    task: 'As the team lead, I needed to resolve the conflict and choose an approach.',
    action: 'I facilitated a structured debate, created evaluation criteria, and built consensus.',
    result: 'We adopted a hybrid approach that incorporated the best of both proposals.',
    reflection: 'I learned that structured decision-making processes help depersonalize conflicts.',
  },
};

export function generateUniqueStory(type = 'Behavioral') {
  const timestamp = Date.now();
  return {
    title: `Test Story ${timestamp}`,
    storyType: type,
    situation: `Test situation for story created at ${timestamp}`,
    task: `Test task description`,
    action: `Test action taken to resolve the situation`,
    result: `Test result of the actions taken`,
    reflection: `Test reflection on what was learned`,
  };
}
