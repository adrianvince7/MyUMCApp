window.notificationService = {
    initialize: function () {
        if (!("Notification" in window)) {
            console.error("This browser does not support desktop notifications");
            return false;
        }
        return true;
    },

    requestPermission: async function () {
        if (!("Notification" in window)) {
            return false;
        }

        try {
            const permission = await Notification.requestPermission();
            return permission === "granted";
        } catch (error) {
            console.error("Error requesting notification permission:", error);
            return false;
        }
    },

    hasPermission: function () {
        if (!("Notification" in window)) {
            return false;
        }
        return Notification.permission === "granted";
    },

    sendNotification: function (title, message, icon) {
        if (!("Notification" in window)) {
            return;
        }

        if (Notification.permission === "granted") {
            const options = {
                body: message,
                icon: icon,
                badge: "/icons/badge.png",
                vibrate: [200, 100, 200],
                tag: new Date().getTime().toString(),
                renotify: true,
                requireInteraction: true,
                silent: false
            };

            const notification = new Notification(title, options);

            notification.onclick = function () {
                window.focus();
                notification.close();
            };
        }
    },

    subscribe: async function () {
        if ('serviceWorker' in navigator && 'PushManager' in window) {
            try {
                const registration = await navigator.serviceWorker.register('/service-worker.js');
                const subscription = await registration.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey: this.urlBase64ToUint8Array(window.vapidPublicKey)
                });

                // Send the subscription to your server
                await fetch('/api/notifications/subscribe', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(subscription)
                });

                return true;
            } catch (error) {
                console.error('Error subscribing to push notifications:', error);
                return false;
            }
        }
        return false;
    },

    unsubscribe: async function () {
        if ('serviceWorker' in navigator) {
            try {
                const registration = await navigator.serviceWorker.ready;
                const subscription = await registration.pushManager.getSubscription();
                
                if (subscription) {
                    await subscription.unsubscribe();
                    
                    // Notify your server about the unsubscription
                    await fetch('/api/notifications/unsubscribe', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify(subscription)
                    });
                }
                return true;
            } catch (error) {
                console.error('Error unsubscribing from push notifications:', error);
                return false;
            }
        }
        return false;
    },

    urlBase64ToUint8Array: function (base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding)
            .replace(/\-/g, '+')
            .replace(/_/g, '/');

        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    }
}; 