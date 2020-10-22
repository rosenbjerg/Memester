import { Component, h } from "preact";
import { Thread } from "../../models";
import Loading from "../../components/loading";
import ky from "ky";
import { route } from "preact-router";
import * as style from "../meme/style.css";

interface State {
    threads?: Thread[];
}

export default class ThreadOverviewPage extends Component<any, State> {
    componentDidMount() {
        ky.get(`/api/threads`)
            .json<Thread[]>()
            .then(threads => this.setState({ threads }));
    }

    render(_, { threads }: State) {
        if (threads === undefined) return <Loading />;

        return (
            <div>
                <h2>Threads</h2>
                <ul>
                    {threads.map(thread => (
                        <li class={`${style.memeLi} truncate`} onClick={() => route(`/${thread.id}`)} key={thread.id}>{thread.name}</li>
                    ))}
                </ul>
            </div>
        );
    }
}
