export function initializeCalendar(dotNetRef) {
    // Initialize any custom calendar functionality
    window.addEventListener('resize', () => {
        // Handle calendar resize if needed
    });

    return {
        // Expose methods that can be called from .NET
        refreshCalendar: () => {
            // Refresh calendar UI if needed
        },
        
        showEventNotification: (title, message) => {
            if ('Notification' in window && Notification.permission === 'granted') {
                new Notification(title, {
                    body: message,
                    icon: '/icons/event.png'
                });
            }
        },

        // Handle virtual meeting links
        joinVirtualMeeting: (url) => {
            window.open(url, '_blank', 'noopener,noreferrer');
        },

        // Add to calendar functionality
        addToCalendar: (event) => {
            const start = new Date(event.startDate);
            const end = new Date(event.endDate);
            const title = encodeURIComponent(event.title);
            const description = encodeURIComponent(event.description);
            const location = encodeURIComponent(event.isVirtual ? event.virtualMeetingUrl : event.location);

            // Generate calendar links
            const googleUrl = `https://calendar.google.com/calendar/render?action=TEMPLATE&text=${title}&dates=${start.toISOString().replace(/[-:]/g, '').split('.')[0]}Z/${end.toISOString().replace(/[-:]/g, '').split('.')[0]}Z&details=${description}&location=${location}`;
            
            // Open Google Calendar in new tab
            window.open(googleUrl, '_blank', 'noopener,noreferrer');
        },

        // Handle recurring events
        generateRecurrencePreview: (pattern) => {
            // Generate preview dates based on recurrence pattern
            const dates = [];
            const start = new Date(pattern.startDate);
            const end = new Date(pattern.endDate);

            let current = new Date(start);
            while (current <= end) {
                dates.push(new Date(current));
                
                switch (pattern.type) {
                    case 'Daily':
                        current.setDate(current.getDate() + pattern.interval);
                        break;
                    case 'Weekly':
                        current.setDate(current.getDate() + (pattern.interval * 7));
                        break;
                    case 'Monthly':
                        current.setMonth(current.getMonth() + pattern.interval);
                        break;
                    case 'Yearly':
                        current.setFullYear(current.getFullYear() + pattern.interval);
                        break;
                }
            }

            return dates;
        }
    };
}