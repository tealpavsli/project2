import { initializeApp } from "https://www.gstatic.com/firebasejs/10.14.1/firebase-app.js";
import {
    getFirestore, doc, getDoc, setDoc
} from "https://www.gstatic.com/firebasejs/10.14.1/firebase-firestore.js";

// ЗАМЕНИТЕ на объект из Firebase Console → Project settings → Your apps
const firebaseConfig = {
    apiKey: "AIzaSyAXCU-nxVoMKC-ydZRCSGrxHNdqBdeI1zM",
    authDomain: "heart-game-d00a9.firebaseapp.com",
    projectId: "heart-game-d00a9",
    storageBucket: "heart-game-d00a9.firebasestorage.app",
    messagingSenderId: "141049504648",
    appId: "1:141049504648:web:26fecfb1bb2f3f09be503d",
};

const app = initializeApp(firebaseConfig);
const db = getFirestore(app);

window.firebaseInterop = {
    // Возвращает рекорд игрока, или 0 если игрок новый
    getRecord: async function (playerName) {
        const ref = doc(db, "players", playerName);
        const snap = await getDoc(ref);
        if (snap.exists()) {
            return snap.data().record ?? 0;
        }
        await setDoc(ref, { record: 0 });
        return 0;
    },

    // Сохраняет рекорд, если он больше текущего сохранённого
    saveRecord: async function (playerName, record) {
        const ref = doc(db, "players", playerName);
        await setDoc(ref, { record: record }, { merge: true });
    }
};